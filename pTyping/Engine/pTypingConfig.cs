using Furball.Engine.Engine.Config;
using Furball.Volpe.Evaluation;

namespace pTyping.Engine; 

public class pTypingConfig : VolpeConfig {
    public override string Name => "ptyping";

    public pTypingConfig() {
        this.Values["test"]    = new Value.String("i am in immense pain AAAAA");
        this.Values["testnum"] = new Value.Number(6969420);
    }
}
